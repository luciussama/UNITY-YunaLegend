"""Regression checks for the landmark failure seen in the Unity study."""
import sys
sys.dont_write_bytecode = True
import unittest

from prepare_sheet import Image, foreground, opaque_box, sole


class LandmarkRegressionTests(unittest.TestCase):
    def test_disconnected_opaque_speck_below_shoe_cannot_move_support(self):
        image = Image.new('RGBA', (200, 200))
        for y in range(20, 120):
            for x in range(80, 120):
                image.putpixel((x, y), (150, 50, 30, 255))
        image.putpixel((100, 170), (255, 0, 0, 255))
        original = image.tobytes()

        self.assertEqual((80, 20, 120, 120), opaque_box(image))
        self.assertEqual((99.5, 120), sole(image))
        self.assertEqual(original, image.tobytes(), 'Landmark analysis must not edit the painting')

    def test_transparent_colored_rgb_is_not_a_foreground_landmark(self):
        image = Image.new('RGBA', (40, 40), (255, 0, 0, 0))
        for y in range(10, 30):
            for x in range(10, 30):
                image.putpixel((x, y), (150, 50, 30, 255))
        self.assertEqual((10, 10, 30, 30), opaque_box(image))

    def test_empty_sheet_cannot_silently_produce_an_anchor(self):
        with self.assertRaises(ValueError):
            foreground(Image.new('RGBA', (40, 40)))


if __name__ == '__main__':
    unittest.main()
