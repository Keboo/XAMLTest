# XAMLTest
![Build](https://github.com/Keboo/XAMLTest/workflows/.NET%20Core/badge.svg)

## Description
XAMLTest is a testing framework designed to allow WPF developers a way to directly "unit test" their XAML. In many ways this library is similar to a UI testing library, with some key differences. Rather than leveraging accessibility or automation APIs, this library is designed to load up a small piece of XAML and provide a simple API to make assertions about the run-time state of the UI. This library is NOT DESIGNED to replace UI testing of WPF apps. Instead, it serves as a helpful tool for WPF library and theme developers to have a mechanism to effectively write tests for their XAML.

## Versioning
After the 1.0.0 release, this library will follow [Semantic Versioning](https://semver.org/). However, in the interim while developing the initial release all 0.x.x versions should be considered alpha releases and all APIs are subject to change without notice.

## Samples?
See the tests (XAMLTests.Tests) for samples of how tests can be setup. [Material Design In XAML Toolkit](https://github.com/MaterialDesignInXAML/MaterialDesignInXamlToolkit) is also leveraging this library for the purpose of testing its various styles and templates. You can see samples of its tests [here](https://github.com/MaterialDesignInXAML/MaterialDesignInXamlToolkit/tree/master/MaterialDesignThemes.UITests).
