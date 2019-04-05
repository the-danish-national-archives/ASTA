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

  String.prototype.reduceWhiteSpace = function() {
      return this.replace(/\s+/g, ' ');
  }

  if (!Date.prototype.getFromFormat) {
    Date.prototype.getFromFormat = function(format) {
      var yyyy = this.getFullYear().toString();
      format = format.replace(/yyyy/g, yyyy)
      var MM = (this.getMonth()+1).toString(); 
      format = format.replace(/MM/g, (MM[1]?MM:"0"+MM[0]));
      var dd  = this.getDate().toString();
      format = format.replace(/dd/g, (dd[1]?dd:"0"+dd[0]));
      var hh = this.getHours().toString();
      format = format.replace(/hh/g, (hh[1]?hh:"0"+hh[0]));
      var mm = this.getMinutes().toString();
      format = format.replace(/mm/g, (mm[1]?mm:"0"+mm[0]));
      var ss  = this.getSeconds().toString();
      format = format.replace(/ss/g, (ss[1]?ss:"0"+ss[0]));
      return format;
  }
}