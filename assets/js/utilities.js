/*
Implments custum javascript helper functions where it used by other modules
*/
if (!String.prototype.format) {
    String.prototype.format = function() {
      var args = arguments;
      return this.replace(/{(\d+)}/g, function(match, number) { 
        return typeof args[number] != 'undefined'
          ? args[number]
          : match
        ;
      });
    };
  }

  if (!String.prototype.getFolders) {
    String.prototype.getFolders = function() {
      if(this.indexOf("\\") > -1) {
        return this.split("\\");
      }
      else {
        return this.toString().split("/");;
      }
    }
  }