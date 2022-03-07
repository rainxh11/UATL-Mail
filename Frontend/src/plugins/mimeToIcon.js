import { AsEnumerable } from 'linq-es5'

const FileTypeMap = {
  accdb: {
    extensions: ['accdb', 'mdb']
  },
  archive: {
    extensions: ['7z', 'ace', 'arc', 'arj', 'dmg', 'gz', 'iso', 'lzh', 'pkg', 'rar', 'sit', 'tgz', 'tar', 'z']
  },
  audio: {
    extensions: [
      'aif',
      'aiff',
      'aac',
      'alac',
      'amr',
      'ape',
      'au',
      'awb',
      'dct',
      'dss',
      'dvf',
      'flac',
      'gsm',
      'm4a',
      'm4p',
      'mid',
      'mmf',
      'mp3',
      'oga',
      'ra',
      'rm',
      'wav',
      'wma',
      'wv'
    ]
  },
  calendar: {
    extensions: ['ical', 'icalendar', 'ics', 'ifb', 'vcs']
  },
  csv: {
    extensions: ['csv']
  },
  docx: {
    extensions: ['doc', 'docm', 'docx', 'docb']
  },
  dotx: {
    extensions: ['dot', 'dotm', 'dotx']
  },
  exe: {
    extensions: ['application', 'appref-ms', 'apk', 'app', 'appx', 'exe', 'ipa', 'msi', 'xap']
  },
  font: {
    extensions: ['ttf', 'otf', 'woff']
  },
  html: {
    extensions: ['htm', 'html', 'mht']
  },
  pdf: {
    extensions: ['pdf']
  },
  photo: {
    extensions: [
      'arw',
      'bmp',
      'cr2',
      'crw',
      'dic',
      'dicm',
      'dcm',
      'dcm30',
      'dcr',
      'dds',
      'dib',
      'dng',
      'erf',
      'gif',
      'heic',
      'heif',
      'ico',
      'jfi',
      'jfif',
      'jif',
      'jpe',
      'jpeg',
      'jpg',
      'kdc',
      'mrw',
      'nef',
      'orf',
      'pct',
      'pict',
      'png',
      'pns',
      'psb',
      'psd',
      'raw',
      'tga',
      'tif',
      'tiff',
      'wdp'
    ]
  },
  ppsx: {
    extensions: ['pps', 'ppsm', 'ppsx']
  },
  pptx: {
    extensions: ['ppt', 'pptm', 'pptx', 'sldx', 'sldm']
  },
  rtf: {
    extensions: ['epub', 'gdoc', 'odt', 'rtf', 'wri', 'pages']
  },
  sysfile: {
    extensions: [
      'bak',
      'bin',
      'cab',
      'cache',
      'cat',
      'cer',
      'class',
      'dat',
      'db',
      'dbg',
      'dl_',
      'dll',
      'ithmb',
      'jar',
      'kb',
      'ldt',
      'lrprev',
      'pkpass',
      'ppa',
      'ppam',
      'pdb',
      'rom',
      'thm',
      'thmx',
      'vsl',
      'xla',
      'xlam',
      'xlb',
      'xll'
    ]
  },
  txt: {
    extensions: ['dif', 'diff', 'readme', 'out', 'plist', 'properties', 'text', 'txt']
  },
  vector: {
    extensions: [
      'ai',
      'ait',
      'cvs',
      'dgn',
      'gdraw',
      'pd',
      'emf',
      'eps',
      'fig',
      'ind',
      'indd',
      'indl',
      'indt',
      'indb',
      'ps',
      'svg',
      'svgz',
      'wmf',
      'oxps',
      'xps',
      'xd',
      'sketch'
    ]
  },
  video: {
    extensions: [
      '3g2',
      '3gp',
      '3gp2',
      '3gpp',
      'asf',
      'avi',
      'dvr-ms',
      'flv',
      'm1v',
      'm4v',
      'mkv',
      'mod',
      'mov',
      'mm4p',
      'mp2',
      'mp2v',
      'mp4',
      'mp4v',
      'mpa',
      'mpe',
      'mpeg',
      'mpg',
      'mpv',
      'mpv2',
      'mts',
      'ogg',
      'qt',
      'swf',
      'ts',
      'vob',
      'webm',
      'wlmp',
      'wm',
      'wmv',
      'wmx'
    ]
  },
  xlsx: {
    extensions: ['xlc', 'xls', 'xlsb', 'xlsm', 'xlsx', 'xlw']
  },
  xltx: {
    extensions: ['xlt', 'xltm', 'xltx']
  },
  xml: {
    extensions: ['xaml', 'xml', 'xsl']
  },
  zip: {
    extensions: ['zip']
  }
}

const findExtensionMimeType = (extension) => {
  const type = AsEnumerable(Object.keys(FileTypeMap))
    .GroupBy(x => x, x => FileTypeMap[x].extensions.flat())
    .FirstOrDefault(x => x.flat().includes(extension))

  return type === null ? 'generic' : type.key
}

export const getMimeIcon = (extension) => {
  const type = findExtensionMimeType(extension)
  
  switch (type) {
  default: 
  case 'generic':
    return { type: 'generic', color: 'orange', icon: 'fa-file' }
  case 'accdb': 
    return { type: type, color: '#c94f60', icon: 'fa-database' }
  case 'archive': 
    return { type: type, color: '#fcdb10', icon: 'fa-file-zipper' }
  case 'audio': 
    return { type: type, color: '#e85acd', icon: 'fa-music' }
  case 'calendar': 
    return { type: type, color: '#90baf9', icon: 'fa-calendar-lines' }
  case 'csv': 
    return { type: type, color: '#57bf10', icon: 'fa-file-csv' }
  case 'docx': 
    return { type: type, color: '#185abd', icon: 'fa-file-word' }
  case 'dotx': 
    return { type: type, color: '#185abd', icon: 'fa-file-word' }
  case 'exe': 
    return { type: type, color: '#979593', icon: 'fa-browser' }
  case 'font': 
    return { type: type, color: '#cc6a2d', icon: 'fa-font-case' }
  case 'html': 
    return { type: type, color: '#e85acd', icon: 'fa-html5' }
  case 'pdf': 
    return { type: type, color: '#b30b00', icon: 'fa-file-pdf' }
  case 'photo': 
    return { type: type, color: '#2ec2a0', icon: 'fa-image' }
  case 'ppsx': 
    return { type: type, color: '#ff8f6b', icon: 'fa-file-powerpoint' }
  case 'pptx': 
    return { type: type, color: '#ff8f6b', icon: 'fa-file-powerpoint' }
  case 'rtf': 
    return { type: type, color: '#185abd', icon: 'fa-file-word' }
  case 'sysfile': 
    return { type: type, color: '#979593', icon: 'fa-gear' }
  case 'txt': 
    return { type: type, color: '#285abd', icon: 'fa-file-lines' }
  case 'vector': 
    return { type: type, color: '#aec2a0', icon: 'fa-file-image' }
  case 'video': 
    return { type: type, color: '#6f00ff', icon: 'fa-film-simple' }
  case 'xlsx': 
    return { type: type, color: '#207245', icon: 'fa-file-excel' }
  case 'xltx': 
    return { type: type, color: '#207245', icon: 'fa-file-excel' }
  case 'xml': 
    return { type: type, color: '#548c6c', icon: 'fa-code' }
  case 'zip': 
    return { type: type, color: '#fcdb10', icon: 'fa-file-zipper' }
  }
}