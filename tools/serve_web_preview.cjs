'use strict';
const http = require('node:http');
const fs = require('node:fs');
const path = require('node:path');
const root = fs.realpathSync(path.resolve(process.argv[2] || 'Build/WebPreview'));
const port = Number(process.argv[3] || 8080);
const types = { '.html': 'text/html; charset=utf-8', '.js': 'application/javascript', '.wasm': 'application/wasm', '.data': 'application/octet-stream', '.json': 'application/json', '.png': 'image/png', '.css': 'text/css' };
const server = http.createServer((req, res) => {
  if (req.method === 'POST' && req.url === '/__save_capture__') {
    let body = '';
    req.on('data', chunk => { body += chunk; });
    req.on('end', () => {
      try {
        const dest = path.resolve(__dirname, '..', 'Build', 'canvas-direct.png');
        fs.writeFileSync(dest, Buffer.from(body.replace(/^data:image\/png;base64,/, ''), 'base64'));
        res.writeHead(200, { 'Content-Type': 'text/plain', 'Access-Control-Allow-Origin': '*' });
        res.end('saved');
      } catch (err) {
        res.writeHead(500, { 'Content-Type': 'text/plain' });
        res.end(err.message);
      }
    });
    return;
  }
  if (!['GET', 'HEAD'].includes(req.method)) { res.writeHead(405); res.end(); return; }
  let file;
  try {
    const url = new URL(req.url, 'http://127.0.0.1');
    const target = path.resolve(root, '.' + decodeURIComponent(url.pathname));
    file = fs.realpathSync(fs.statSync(target).isDirectory() ? path.join(target, 'index.html') : target);
    if (file !== root && !file.startsWith(root + path.sep)) { res.writeHead(403); res.end(); return; }
    const size = fs.statSync(file).size;
    res.writeHead(200, { 'Content-Type': types[path.extname(file)] || 'application/octet-stream', 'Content-Length': size, 'Cache-Control': 'no-store', 'Cross-Origin-Opener-Policy': 'same-origin', 'Cross-Origin-Embedder-Policy': 'require-corp' });
    if (req.method === 'HEAD') res.end(); else fs.createReadStream(file).on('error', () => res.destroy()).pipe(res);
  } catch { res.writeHead(404); res.end('Not found'); }
});
server.on('error', error => { console.error(error.message); process.exitCode = 1; });
server.listen(port, '127.0.0.1', () => console.log(`Unity preview: http://127.0.0.1:${port}`));
