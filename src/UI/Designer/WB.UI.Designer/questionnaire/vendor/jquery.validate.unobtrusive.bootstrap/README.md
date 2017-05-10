jquery.validate.unobtrusive.bootstrap
=====================================

A jQuery Validate Unobtrusive extension for Bootstrap 3

If you liked it, buy me a beer!

<a href='https://pledgie.com/campaigns/25585'><img alt='Click here to lend your support to: jquery.validate.unobtrusive.bootstrap and make a donation at pledgie.com !' src='https://pledgie.com/campaigns/25585.png?skin_name=chrome' border='0' ></a>

## Where can I get it?

You can get it on [Nuget](http://nuget.org) from the package manager console:
```
PM> Install-Package jquery.validate.unobtrusive.bootstrap
```

or on Bower:
```
$ bower install jquery.validate.unobtrusive.bootstrap
```
## How to use?

Just include the javascript after jquery.validate.unobtrusive and that's it!

```html
<script src="jquery-1.10.2.min.js"></script>
<script src="bootstrap.min.js"></script>
<script src="jquery.validate.min.js"></script>
<script src="jquery.validate.unobtrusive.min.js"></script>
<script src="jquery.validate.unobtrusive.bootstrap.min.js"></script>
```

## Other functions

If your form has dynamic added elements that need validation, you can call `.validateBootstrap(true)` to rebuild all validations with the new elements included
```html
<script>
  $('form').validateBootstrap(true);
</script>
```

<hr />

## License

Licensed under the [MIT License](http://www.opensource.org/licenses/mit-license.php).
