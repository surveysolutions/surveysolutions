# group-array [![NPM version](https://badge.fury.io/js/group-array.svg)](http://badge.fury.io/js/group-array)  [![Build Status](https://travis-ci.org/doowb/group-array.svg)](https://travis-ci.org/doowb/group-array)

> Group array of objects into lists.

## Install

Install with [npm](https://www.npmjs.com/)

```sh
$ npm i group-array --save
```

## Usage

```js
var groupArray = require('group-array');
```

## Examples

```js
var arr = [
  {tag: 'one', content: 'A'},
  {tag: 'one', content: 'B'},
  {tag: 'two', content: 'C'},
  {tag: 'two', content: 'D'},
  {tag: 'three', content: 'E'},
  {tag: 'three', content: 'F'}
];

// group by the `tag` property
groupArray(arr, 'tag');
```

**results in:**

```js
{
  one: [
    {tag: 'one', content: 'A'},
    {tag: 'one', content: 'B'}
  ],
  two: [
    {tag: 'two', content: 'C'},
    {tag: 'two', content: 'D'}
  ],
  three: [
    {tag: 'three', content: 'E'},
    {tag: 'three', content: 'F'}
  ]
}
```

**Group by multiple, deeply nested properties**

```js
// given an array of object, like blog posts...
var arr = [
  { data: { year: '2014', tag: 'one', month: 'jan', day: '01'}, content: '...'},
  { data: { year: '2014', tag: 'one', month: 'jan', day: '01'}, content: '...'},
  { data: { year: '2014', tag: 'one', month: 'jan', day: '02'}, content: '...'},
  { data: { year: '2014', tag: 'one', month: 'jan', day: '02'}, content: '...'},
  { data: { year: '2014', tag: 'one', month: 'feb', day: '10'}, content: '...'},
  { data: { year: '2014', tag: 'one', month: 'feb', day: '10'}, content: '...'},
  { data: { year: '2014', tag: 'one', month: 'feb', day: '12'}, content: '...'},
  { data: { year: '2014', tag: 'one', month: 'feb', day: '12'}, content: '...'},
  { data: { year: '2014', tag: 'two', month: 'jan', day: '14'}, content: '...'},
  { data: { year: '2014', tag: 'two', month: 'jan', day: '14'}, content: '...'},
  { data: { year: '2014', tag: 'two', month: 'jan', day: '16'}, content: '...'},
  { data: { year: '2014', tag: 'two', month: 'jan', day: '16'}, content: '...'},
  { data: { year: '2014', tag: 'two', month: 'feb', day: '18'}, content: '...'},
  { data: { year: '2015', tag: 'two', month: 'feb', day: '18'}, content: '...'},
  { data: { year: '2015', tag: 'two', month: 'feb', day: '10'}, content: '...'},
  { data: { year: '2015', tag: 'two', month: 'feb', day: '10'}, content: '...'},
  { data: { year: '2015', tag: 'three', month: 'jan', day: '01'}, content: '...'},
  { data: { year: '2015', tag: 'three', month: 'jan', day: '01'}, content: '...'},
  { data: { year: '2015', tag: 'three', month: 'jan', day: '02'}, content: '...'},
  { data: { year: '2015', tag: 'three', month: 'jan', day: '02'}, content: '...'},
  { data: { year: '2015', tag: 'three', month: 'feb', day: '01'}, content: '...'},
  { data: { year: '2015', tag: 'three', month: 'feb', day: '01'}, content: '...'},
  { data: { year: '2015', tag: 'three', month: 'feb', day: '02'}, content: '...'},
  { data: { year: '2015', tag: 'three', month: 'feb', day: '02'}, content: '...'}
]
```

Pass a list or array of properties:

```js
groupArray(arr, 'data.year', 'data.tag', 'data.month', 'data.day');
```

**Results in something like this: (abbreviated)**

```js
{
  '2014':
   { one:
      { jan:
         { '01':
            [ { data: { year: '2014', ... },
              { data: { year: '2014', ... } ],
           '02':
            [ { data: { year: '2014', ... },
              { data: { year: '2014', ... } ] },
        feb:
         { '10':
            [ { data: { year: '2014', ... },
              { data: { year: '2014', ... } ],
           '12':
            [ { data: { year: '2014', ... },
              { data: { year: '2014', ... } ] } },
     two:
      { jan:
         { '14':
            [ { data: { year: '2014', ... },
              { data: { year: '2014', ... } ],
           '16':
            [ { data: { year: '2014', ... },
              { data: { year: '2014', ... } ] },
        feb:
         { '18':
            [ { data: { year: '2014', ... } ] } } },
  '2015':
   { two:
      { feb:
         { '10':
            [ { data: { year: '2015', ... },
              { data: { year: '2015', ... } ],
           '18':
            [ { data: { year: '2015', ... } ] } },
     three:
      { jan:
         { '01':
            [ { data: { year: '2015', ... },
              { data: { year: '2015', ... } ],
           '02':
            [ { data: { year: '2015', ... },
              { data: { year: '2015', ... } ] },
        feb:
         { '01':
            [ { data: { year: '2015', ... },
              { data: { year: '2015', ... } ],
           '02':
            [ { data: { year: '2015', ... },
              { data: { year: '2015', ... } ] } } }};
```

## Related projects

* [arr-reduce](https://github.com/jonschlinkert/arr-reduce): Fast array reduce that also loops over sparse elements.
* [group-object](https://github.com/doowb/group-object): Group object keys and values into lists.
* [get-value](https://github.com/jonschlinkert/get-value): Use property paths (`  a.b.c`) to get a nested value from an object.
* [union-value](https://github.com/jonschlinkert/union-value): Set an array of unique values as the property of an object. Supports setting deeply… [more](https://github.com/jonschlinkert/union-value)

## Running tests

Install dev dependencies:

```sh
$ npm i -d && npm test
```

## Contributing

Pull requests and stars are always welcome. For bugs and feature requests, [please create an issue](https://github.com/doowb/group-array/issues/new)

## Author

**Brian Woodward**

+ [github/doowb](https://github.com/doowb)
+ [twitter/doowb](http://twitter.com/doowb)

## License

Copyright © 2015 Brian Woodward
Released under the MIT license.

***

_This file was generated by [verb-cli](https://github.com/assemble/verb-cli) on July 22, 2015._