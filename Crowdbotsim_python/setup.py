# -*- coding: utf-8 -*-

# Learn more: https://github.com/kennethreitz/setup.py

from setuptools import setup, find_packages


with open('README.rst') as f:
    readme = f.read()

with open('LICENSE') as f:
    license = f.read()

setup(
    name='crowdbotsimcontrol-ethrl',
    version='0.1.0',
    description='python API to control CrowdBotSim executable - updated for use with ETH RL projects',
    long_description=readme,
    author='Daniel Dugas',
    author_email='exodaniel@gmail.com',
    license=license,
    packages=find_packages(exclude=('tests', 'docs'))
)

