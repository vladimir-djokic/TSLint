# What is it?

Visual Studio 2017 extension for liting **Typescript** files using `tslint`.

![TSLint](TSLint/Resources/preview.png)

# How does it work?

When `.ts` is opened or saved, locally installed `tslint` is run in the background and
code in the editor is underlined (marked) based on the `tslint` findings. Hovering over the underline
gives additional info in the form of a tooltip and warnings and errors are listed in the **Error List**.

# Roadmap

- [x] Support both warnings and erros
- [x] Support the **Error List**
- [ ] Support automated fixes
- [ ] Optimizations
