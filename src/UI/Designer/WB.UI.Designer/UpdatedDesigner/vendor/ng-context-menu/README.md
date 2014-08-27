# [ng-context-menu](http://ianwalter.github.io/ng-context-menu/)
*An AngularJS directive to display a context menu when a right-click event is triggered*

[![Code Climate](https://codeclimate.com/github/ianwalter/ng-context-menu.png)](https://codeclimate.com/github/ianwalter/ng-context-menu)

This project was built using [ng-boilerplate](https://github.com/ianwalter/ng-boilerplate)!

#### Step 1: Install ng-context-menu

Install using Bower:

```
bower install ng-context-menu --save
```

Include ng-context-menu.min.js in your app.

#### Step 2: Load the ng-context-menu module

```javascript
var app = angular.module('menu-demo', ['ngRoute', 'ng-context-menu'])
```

#### Step 3: Add the context-menu directive to a DOM element

```html
<div context-menu class="panel panel-default position-fixed" data-target="myMenu"
     ng-class="{ 'highlight': highlight, 'expanded' : expanded }">
  ...
</div>
```

#### Step 4: Make sure your menu is has the ```position: fixed``` CSS property

As you can see in the demo, I just created a class called position-fixed and added the property:

```css
.position-fixed {
  position: fixed;
}
```

#### Disabling the contextmenu

If you need to disable the contextmenu in certain circumstances, you can add an expression to the
 ```context-menu-disabled``` attribute. If the expression evaluates to true, the contextmenu will be
 disabled, for example, ```context-menu-disabled="1 === 1"```

That's it, I hope you find this useful!

«–– [Ian](http://ianvonwalter.com)
