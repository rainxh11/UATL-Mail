const glob = require("glob");
const fs = require('fs');

function generateUUID() { // Public Domain/MIT
  var d = new Date().getTime();//Timestamp
  var d2 = ((typeof performance !== 'undefined') && performance.now && (performance.now()*1000)) || 0;//Time in microseconds since page-load or 0 if unsupported
  return 'xxxxxxxx-xxxx-4xxx-yxxx-xxxxxxxxxxxx'.replace(/[xy]/g, function(c) {
      var r = Math.random() * 16;//random number between 0 and 16
      if(d > 0){//Use timestamp until depleted
          r = (d + r)%16 | 0;
          d = Math.floor(d/16);
      } else {//Use microseconds since page-load if supported
          r = (d2 + r)%16 | 0;
          d2 = Math.floor(d2/16);
      }
      return (c === 'x' ? r : (r & 0x3 | 0x8)).toString(16);
  });
}

var getDirectories = function (src, callback) {
  glob(src + '/**/*', callback);
};
getDirectories('./dist', function (err, res) {
  if (err) {
    console.log('Error', err);
  } else {
    const files = res
    .filter(x => x.match(/(.js|.png|.otf|.woff2|.ttf|.html|.css|.svg|.jpg)$/gim) !== null)
    .map(x => x.replace('./dist', ''))

    console.log(files)

    try {
      //fs.writeFileSync('./dist/files.json', JSON.stringify(files))
      var swFile = fs.readFileSync('./service-worker.js',{ encoding: 'utf8' });
      swFile = swFile.replace('\'#FILES\'', JSON.stringify(files).replaceAll('"', '\''));
      swFile = swFile.replace('#CACHENAME', generateUUID());

      fs.writeFileSync('./dist/service-worker.js', swFile)

    } catch (err) {
      console.error(err)
    }  
    
  }
});

