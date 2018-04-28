# Changelog

## Version 0.5.0

**2018-04-28**

- If `tsc` and `tsconfig.json` are detected, `tslint` is run with `--project` flag providing type checking support.

## Version 0.4.5

**2018-03-21**

- Merge [#15 Fixed NullReferenceException](https://github.com/vladeck/TSLint/pull/15) from Mads Kristensen (fixes numerous `NullReferenceException`(s) in `ErrorListHelper`)

## Version 0.4.4

**2018-02-26**

- Resolve [#14: Linting .js files as well as .ts](https://github.com/vladeck/TSLint/issues/14).

## Version 0.4.3

**2017-12-13**

- Resolve [#9: Some files are not analyzed](https://github.com/vladeck/TSLint/issues/9).
- Performance increase by not stalling processes by reading output at the wrong time.

## Version 0.4.2

**2017-10-24**

- Resolve [#7 Visual Studio 2017 Crashing](https://github.com/vladeck/TSLint/issues/7).
- Resolve crashing when `tslint` errors are spanning multiple lines.

## Version 0.4.1

**2017-06-21**

- Support for spaces in filenames (thanks to [Holger Jeromin](https://github.com/HolgerJeromin) for [PR #4](https://github.com/vladeck/TSLint/pull/4)).

## Version 0.4

**2017-06-07**

- Resolve [#3 System.ArgumentException on a single ts file with no project/solution](https://github.com/vladeck/TSLint/issues/3)
(support for dragged or explicitly opened `.ts` files).
- Support linting even when there is no solution/project associated to the opened `.ts` file
(when the `tslint.cmd` and `tslint.json` are somewhere in the path of the opened `.ts` file).

## Version 0.3.1

**2017-06-07**

- Resolve [#2 Support configuration files](https://github.com/vladeck/TSLint/issues/2) (better `extends` support).

## Version 0.3

**2017-05-31**

- Add support for Visual Studio 2015.

## Version 0.2.2

**2017-05-25**

- Cache project/solution and tslint.cmd path pairs when linting (small performance improvement).

## Version 0.2.1

**2017-05-23**

- Support local installation of `tslint` relative to the opened document's solution (project's `tslint` takes precedence).

## Version 0.2

**2017-05-23**

- Support local installation of `tslint` relative to the opened document's project.

## Version 0.1

**2017-05-23**

- First release.
