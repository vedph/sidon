# Sidon Tool

This is a use-and-throw tool to prepare Sidonius Apollinaris epistles for import into a Cadmus based system.

## Parsing Text

The input document is a plain text document with these features:

- each file represents a book and is a set of documents (letters, in this case).

For each document:

- the first line is the title, e.g.:

```txt
SIDONII APOLLINARIS EPISTULARUM LIBER PRIMUS, ED. LUETJOHANN
```

- each document (letter) is a blank line followed by `^EPISTULA\s+([IVXLC]+)`. We want to extract the letter number and convert it from Roman into a numeric value.
- this paragraph is followed by the dedicatee, e.g.

```txt
Sidonius Constantio suo salutem.
```

- then, the content follows. As for content:

  - each paragraph starting with `^\d+\.\s+` is a prose paragraph. It runs until the next prose/poetic paragraph, thus including itself and eventually other following (not numbered) paragraphs.
  - a poetic paragraph starts with space(s) because it's indented. It may start with `^\s*\(\d+\)\s*` (verse number in brackets), which must be removed.
  - a special case of poetic paragraph starts with no space nor number, but is followed by a paragraph starting with 3 spaces. In this case it's the first verse of a distichon.
  - exceptions to these rules are marked with an initial character: `@`=poetry, `#`=prose.

## CLI

(1) dump for diagnostic purposes, e.g.:

./sidon dump c:\users\dfusi\desktop\sid\sidon*.txt c:\users\dfusi\desktop\dump\

(2) import into database, e.g.:

./sidon import c:\users\dfusi\desktop\sid\sidon*.txt cadmus-sidon

Note: the quickest way to create a new database is starting Cadmus Sidon API to let it seed its metadata, and then delete all the items/parts collections and run this import command.

## History

- 2022-11-10: upgraded to NET 7.
- 2022-08-21: added asterisk in imported item title and flags=8 when its block(s) contain poetry.
