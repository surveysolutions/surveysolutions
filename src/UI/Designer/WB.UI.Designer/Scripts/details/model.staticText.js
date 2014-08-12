define('model.staticText',
   ['ko'],
   function (ko) {

       var _dc = null,
           StaticText = function () {
               var self = this;
               self.id = ko.observable(Math.uuid());

               self.title = ko.observable();
               self.parent = ko.observable();

               self.type = ko.observable("StaticTextView"); // Object type
               self.template = "StaticTextView"; // tempate id in html file
       };

       StaticText.Nullo = new StaticText().id(0).title('Title').type('StaticTextView');
       StaticText.Nullo.isNullo = true;
       
       return StaticText;
   });