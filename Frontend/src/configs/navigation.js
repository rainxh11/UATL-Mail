export default {
  // main navigation - side menu
  menu: [
    {
      text: 'Command Center',
      role: ['admin','user'],
      key: 'menu.mailbox',
      items: [
        { icon: 'fa-duotone fa-mailbox', key: 'menu.studyPage', text: 'Mail Box', link: '/mailbox' , role: ['user','admin']  }
      ]
    }],

  // footer links
  footer: [{
    text: 'Docs',
    key: 'menu.docs',
    href: 'https://vuetifyjs.com',
    target: '_blank'
  }]
}
