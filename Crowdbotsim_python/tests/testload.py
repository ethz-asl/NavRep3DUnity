# -*- coding: utf-8 -*-

import context
from context import crowdbotsimcontrol
import unittest


class AdvancedTestSuite(unittest.TestCase):
    """Advanced test cases."""

    def test(self):
        self.assertIsNone(crowdbotsimcontrol.load())


if __name__ == '__main__':
    unittest.main()
