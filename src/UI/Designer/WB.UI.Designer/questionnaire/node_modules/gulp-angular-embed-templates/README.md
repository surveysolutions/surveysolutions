# gulp-angular-embed-templates

> gulp plugin to include the contents of angular templates inside directive's code

----

Plugin searches for `templateUrl: {template url}` and replace it with `template: {minified template content}`. To archive this template first minified with [minimize](https://www.npmjs.com/package/minimize)

Nearest neighbours are:

*   *gulp-angular-templates* - good for single page applications, combine all templates in one module. *gulp-angular-embed-templates* is better for **multi page applications**, where different pages use different set of angular directives so combining all templates in one is not an option. For single page applications they are similar but *angular-inject-templates* doesn't forces you to change your code for using some additional module: just replace template reference with the template code.
*   *gulp-include-file* - can be used for the same purpose (file include) with *minimize* plugin as transform functions. *gulp-angular-embed-templates* do all of this out of the box.

## Versions / Release Notes

[CHANGELOG on GitHub](https://github.com/laxa1986/gulp-angular-embed-templates/blob/master/CHANGELOG.md)

## Install

    npm install --save-dev gulp-angular-embed-templates

## Usage (Angular 1.x)

Given the following file structure

```javascript
src
+-hello-world
  |-hello-world-directive.js
  +-hello-world-template.html
```

`hello-world-directive.js`:

```javascript
angular.module('test').directive('helloWorld', function () {
    return {
        restrict: 'E',
        // relative path to template
        templateUrl: 'hello-world-template.html'
    };
});
```

`hello-world-template.html`:

```html
<strong>
    Hello world!
</strong>
```

`gulpfile.js`:

```javascript
var gulp = require('gulp');
var embedTemplates = require('gulp-angular-embed-templates');

gulp.task('js:build', function () {
    gulp.src('src/scripts/**/*.js')
        .pipe(embedTemplates())
        .pipe(gulp.dest('./dist'));
});
```

*gulp-angular-embed-templates* will generate the following file:

```javascript
angular.module('test').directive('helloWorld', function () {
    return {
        restrict: 'E',
        template:'<strong>Hello world!</strong>'
    };
});
```

## Usage (Angular 2.0)

Given the following file structure

```javascript
src
+-hello-world
  |-hello-world-component.ts
  +-hello-world-template.html
```

`hello-world-component.ts`:

```javascript
class Component extends Directive {
  restrict: string = "E";
  controller: Controller;
  controllerAs: string = "vm";
  templateUrl: string = "angular2-template.html";
}
// or
@View({
    ...
    templateUrl: 'angular2-template.html'
})
```

`angular2-template.html`:

```html
<task-cmp [model]="task" (complete)="onCmpl(task)">
    {{index}}
</task-cmp>
```

`gulpfile.js`:

```javascript
var gulp = require('gulp');
var embedTemplates = require('gulp-angular-embed-templates');

gulp.task('js:build', function () {
    gulp.src('src/scripts/**/*.ts') // also can use *.js files
        .pipe(embedTemplates({sourceType:'ts'}))
        .pipe(gulp.dest('./dist'));
});
```

*gulp-angular-embed-templates* will generate the following file:

```javascript
class Component extends Directive {
  restrict: string = "E";
  controller: Controller;
  controllerAs: string = "vm";
  template:string='<task-cmp [model]="task" (complete)="onCmpl(task)">{{index}}</task-cmp>';
}
// or
@View({
    ...
    template:'<task-cmp [model]="task" (complete)="onCmpl(task)">{{index}}</task-cmp>'
})
```

**Note**: call _embedTemplates_ before source maps initialization.

## API

### embedTemplates(options)

#### options.sourceType
Type: `String`. Default value: 'js'. Available values:
- 'js' both for Angular 1.x syntax `templateUrl: 'path'` and Angular 2.x syntax `@View({templateUrl: 'path'})`
- 'ts' additionally support Angular 2.x TypeScript syntax `class Component {templateUrl: string = 'path'}`

#### options.basePath
Type: `String`. By default plugin use path specified in 'templateUrl' as a relative path to corresponding '.js' file (file with 'templateUrl'). This option allow to specify another basePath to search templates as 'basePath'+'templateUrl'

#### skip one template embedding
The easiest way to skip one concrete is just add some comment like `/*!*/` between templateUrl and template path, like this: `templateUrl: /*!*/ '/template-path.html'`

#### options.skipFiles
Type: `RegExp` or `Function`. By default: do not skip any files. RegExp can test file name to skip template embedding, but this file still be passed in general gulp pipe and be visible for all follow plugins. Function can be used for more detail filtering. Example: `function(file) {return file.path.endsWith('-skip-directive.js');}`

#### options.skipTemplates
Type: `RegExp` or `Function`. By default: do not skip any templates. RegExp can test concrete templateUrl to skip it (like `/\-large\.html$/`). Function can be used for more detail filtering. Example: `function(templatePath, fileContext) {return templatePath.endsWith('-large.html');}`

#### options.minimize
Type: `Object`. Default value: {parser: customParser}

settings to pass in minimize plugin. Please see all settings on [minimize official page](https://www.npmjs.com/package/minimize). Please don't specify key 'parser' because it already used for internal purposes

#### options.skipErrors
Type: `Boolean`. Default value: 'false'

should plugin brake on errors (file not found, error in minification) or skip errors (warn in logs) and go to next template

#### options.jsEncoding
Type: `String`. Default value: 'utf-8'

js files encoding (angular directives)

#### options.templateEncoding
Type: `String`. Default value: 'utf-8'

angular template files encoding

#### options.maxSize
Type: `Number`. Not specified by default (templates of any size allowed)

define the max size limit in bytes for the template be embedded. Ignore templates which size exceed this limit

## License
This module is released under the MIT license.


