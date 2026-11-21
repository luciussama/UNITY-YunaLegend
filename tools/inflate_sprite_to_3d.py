#!/usr/bin/env python3
"""Generates a smooth volumetric 3D relief .obj mesh from a transparent PNG sprite."""

import math
import os
import struct
import subprocess
import sys
import zlib


def read_png_rgba(path):
    with open(path, "rb") as f:
        sig = f.read(8)
        if sig != b"\x89PNG\r\n\x1a\n":
            raise ValueError(f"Not a PNG: {path}")
        width = height = bit_depth = color_type = 0
        idat = bytearray()
        while True:
            hdr = f.read(8)
            if len(hdr) < 8:
                break
            length, ctype = struct.unpack(">I4s", hdr)
            data = f.read(length)
            f.read(4)  # crc
            if ctype == b"IHDR":
                width, height, bit_depth, color_type = struct.unpack(">IIBB", data[:10])
            elif ctype == b"IDAT":
                idat.extend(data)
            elif ctype == b"IEND":
                break

    if bit_depth != 8 or color_type not in (2, 6):
        raise ValueError(f"Unsupported PNG format depth={bit_depth} type={color_type}")

    bpp = 4 if color_type == 6 else 3
    raw = zlib.decompress(bytes(idat))
    stride = width * bpp
    recon = bytearray(height * stride)

    def paeth(a, b, c):
        p = a + b - c
        pa, pb, pc = abs(p - a), abs(p - b), abs(p - c)
        if pa <= pb and pa <= pc:
            return a
        return b if pb <= pc else c

    pos = 0
    for y in range(height):
        ftype = raw[pos]
        pos += 1
        row_start = y * stride
        for i in range(stride):
            x = raw[pos]
            pos += 1
            a = recon[row_start + i - bpp] if i >= bpp else 0
            b = recon[row_start - stride + i] if y > 0 else 0
            c = recon[row_start - stride + i - bpp] if (y > 0 and i >= bpp) else 0
            if ftype == 0:
                val = x
            elif ftype == 1:
                val = (x + a) & 0xFF
            elif ftype == 2:
                val = (x + b) & 0xFF
            elif ftype == 3:
                val = (x + ((a + b) >> 1)) & 0xFF
            elif ftype == 4:
                val = (x + paeth(a, b, c)) & 0xFF
            else:
                val = x
            recon[row_start + i] = val

    rgba = []
    for y in range(height):
        row = []
        base = y * stride
        for x in range(width):
            idx = base + x * bpp
            a = recon[idx + 3] if bpp == 4 else 255
            row.append(a)
        rgba.append(row)
    return width, height, rgba


def compute_distance_field(alpha_grid, w, h):
    # Chamfer 3-4 distance transform in 2 passes (fast, pure Python)
    inf = 1e9
    dist = [[0.0 if alpha_grid[y][x] < 96 else inf for x in range(w)] for y in range(h)]
    # Forward pass
    for y in range(h):
        for x in range(w):
            d = dist[y][x]
            if d == 0.0:
                continue
            if x > 0:
                d = min(d, dist[y][x - 1] + 1.0)
            if y > 0:
                d = min(d, dist[y - 1][x] + 1.0)
                if x > 0:
                    d = min(d, dist[y - 1][x - 1] + 1.414)
                if x + 1 < w:
                    d = min(d, dist[y - 1][x + 1] + 1.414)
            dist[y][x] = d
    # Backward pass
    max_d = 0.0
    for y in range(h - 1, -1, -1):
        for x in range(w - 1, -1, -1):
            d = dist[y][x]
            if d == 0.0:
                continue
            if x + 1 < w:
                d = min(d, dist[y][x + 1] + 1.0)
            if y + 1 < h:
                d = min(d, dist[y + 1][x] + 1.0)
                if x > 0:
                    d = min(d, dist[y + 1][x - 1] + 1.414)
                if x + 1 < w:
                    d = min(d, dist[y + 1][x + 1] + 1.414)
            dist[y][x] = d
            if d > max_d:
                max_d = d
    return dist, max(1.0, max_d)


def build_volumetric_obj(src_png, out_obj, grid_res=96, thickness=0.22):
    tmp_png = "/tmp/_inflate_tmp.png"
    subprocess.run(["sips", "-z", str(grid_res), str(grid_res), src_png, "--out", tmp_png],
                   check=True, stdout=subprocess.DEVNULL, stderr=subprocess.DEVNULL)

    orig_w, orig_h, _ = read_png_rgba(src_png)
    aspect = orig_w / float(orig_h)

    w, h, alpha = read_png_rgba(tmp_png)
    dist, max_d = compute_distance_field(alpha, w, h)

    # Find opaque bounding box to plant bottom at y=0
    min_y, max_y = h, 0
    for y in range(h):
        for x in range(w):
            if alpha[y][x] >= 96:
                min_y = min(min_y, y)
                max_y = max(max_y, y)
    span_y = max(1, max_y - min_y)

    # Smooth z heightmap
    zmap = [[0.0 for _ in range(w)] for _ in range(h)]
    for y in range(h):
        for x in range(w):
            if dist[y][x] > 0.0:
                norm = min(1.0, dist[y][x] / (max_d * 0.72))
                zmap[y][x] = thickness * math.sqrt(norm)

    # Light smoothing pass on zmap
    for _ in range(2):
        nz = [row[:] for row in zmap]
        for y in range(1, h - 1):
            for x in range(1, w - 1):
                if zmap[y][x] > 0.0:
                    nz[y][x] = (
                        zmap[y][x] * 4.0
                        + zmap[y - 1][x]
                        + zmap[y + 1][x]
                        + zmap[y][x - 1]
                        + zmap[y][x + 1]
                    ) / 8.0
        zmap = nz

    verts = []
    uvs = []
    front_idx = [[-1] * w for _ in range(h)]
    back_idx = [[-1] * w for _ in range(h)]

    def add_vert(gx, gy, gz):
        # Convert image coords (y=0 top) -> Unity world (y=0 bottom, height=1.0)
        # Negate X so Unity OBJ importer X-flip preserves original orientation!
        wx = -((gx / (w - 1.0)) - 0.5) * aspect * (h / float(span_y))
        wy = (max_y - gy) / float(span_y)
        u = gx / (w - 1.0)
        v = 1.0 - (gy / (h - 1.0))
        verts.append((wx, wy, gz))
        uvs.append((u, v))
        return len(verts)

    for y in range(h):
        for x in range(w):
            if alpha[y][x] >= 96:
                z = zmap[y][x]
                front_idx[y][x] = add_vert(x, y, z)
                back_idx[y][x] = add_vert(x, y, -z)

    faces = []
    for y in range(h - 1):
        for x in range(w - 1):
            f00 = front_idx[y][x]
            f10 = front_idx[y][x + 1]
            f01 = front_idx[y + 1][x]
            f11 = front_idx[y + 1][x + 1]
            if f00 != -1 and f10 != -1 and f01 != -1 and f11 != -1:
                # Front (+Z)
                faces.append((f00, f10, f11))
                faces.append((f00, f11, f01))
                # Back (-Z)
                b00 = back_idx[y][x]
                b10 = back_idx[y][x + 1]
                b01 = back_idx[y + 1][x]
                b11 = back_idx[y + 1][x + 1]
                faces.append((b00, b11, b10))
                faces.append((b00, b01, b11))

    # Stitch boundary rim between front and back
    for y in range(h - 1):
        for x in range(w - 1):
            # Horizontal edge (x,y)-(x+1,y)
            if front_idx[y][x] != -1 and front_idx[y][x + 1] != -1:
                above_empty = (y == 0) or (front_idx[y - 1][x] == -1 and front_idx[y - 1][x + 1] == -1)
                below_empty = (y + 1 >= h) or (front_idx[y + 1][x] == -1 and front_idx[y + 1][x + 1] == -1)
                if above_empty:
                    faces.append((front_idx[y][x], back_idx[y][x], back_idx[y][x + 1]))
                    faces.append((front_idx[y][x], back_idx[y][x + 1], front_idx[y][x + 1]))
                if below_empty:
                    faces.append((front_idx[y][x], front_idx[y][x + 1], back_idx[y][x + 1]))
                    faces.append((front_idx[y][x], back_idx[y][x + 1], back_idx[y][x]))
            # Vertical edge (x,y)-(x,y+1)
            if front_idx[y][x] != -1 and front_idx[y + 1][x] != -1:
                left_empty = (x == 0) or (front_idx[y][x - 1] == -1 and front_idx[y + 1][x - 1] == -1)
                right_empty = (x + 1 >= w) or (front_idx[y][x + 1] == -1 and front_idx[y + 1][x + 1] == -1)
                if left_empty:
                    faces.append((front_idx[y][x], front_idx[y + 1][x], back_idx[y + 1][x]))
                    faces.append((front_idx[y][x], back_idx[y + 1][x], back_idx[y][x]))
                if right_empty:
                    faces.append((front_idx[y][x], back_idx[y][x], back_idx[y + 1][x]))
                    faces.append((front_idx[y][x], back_idx[y + 1][x], front_idx[y + 1][x]))

    # Compute smooth vertex normals
    normals = [[0.0, 0.0, 0.0] for _ in verts]
    for a, b, c in faces:
        va, vb, vc = verts[a - 1], verts[b - 1], verts[c - 1]
        ux, uy, uz = vb[0] - va[0], vb[1] - va[1], vb[2] - va[2]
        vx, vy, vz = vc[0] - va[0], vc[1] - va[1], vc[2] - va[2]
        nx = uy * vz - uz * vy
        ny = uz * vx - ux * vz
        nz = ux * vy - uy * vx
        for idx in (a - 1, b - 1, c - 1):
            normals[idx][0] += nx
            normals[idx][1] += ny
            normals[idx][2] += nz

    os.makedirs(os.path.dirname(out_obj), exist_ok=True)
    with open(out_obj, "w") as out:
        out.write(f"# Volumetric 3D relief generated from {src_png}\n")
        for vx, vy, vz in verts:
            out.write(f"v {vx:.6f} {vy:.6f} {vz:.6f}\n")
        for u, v in uvs:
            out.write(f"vt {u:.6f} {v:.6f}\n")
        for nx, ny, nz in normals:
            l = math.sqrt(nx * nx + ny * ny + nz * nz)
            if l > 1e-8:
                out.write(f"vn {nx / l:.6f} {ny / l:.6f} {nz / l:.6f}\n")
            else:
                out.write("vn 0.0 0.0 1.0\n")
        for a, b, c in faces:
            out.write(f"f {a}/{a}/{a} {b}/{b}/{b} {c}/{c}/{c}\n")

    print(f"Generated {out_obj}: {len(verts)} verts, {len(faces)} tris")


if __name__ == "__main__":
    build_volumetric_obj(sys.argv[1], sys.argv[2], grid_res=int(sys.argv[3]) if len(sys.argv) > 3 else 96)
