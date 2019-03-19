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
  
  if (!String.prototype.normlizePath) {
    String.prototype.normlizePath = function() {
      if(this.indexOf("\\") > -1) {
        var folders = this.split("\\");
        folders = folders.slice(1,folders.length);        
        return "/{0}".format(folders.join("/"));
      }
      else {
        return this.toString();
      }
    }
  }