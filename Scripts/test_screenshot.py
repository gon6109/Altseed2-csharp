import unittest
import os
import time
import shutil
import filecmp
import base64
import glob
import json

class ScreenShotTest(unittest.TestCase):
    def test_screenshots(self):
        generated_file_paths = glob.glob('../TestResult/*.png')

        for path in generated_file_paths:
            name = os.path.basename(path)
            self.assertTrue(filecmp.cmp('../test-windows/' + name, path), name + ' is not equal')

if __name__ == '__main__':
    unittest.main()

