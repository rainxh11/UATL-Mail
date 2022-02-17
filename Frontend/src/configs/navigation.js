export default {
  // main navigation - side menu
  menu: [
    {
      text: 'Command Center',
      role: ['Admin','User'],
      key: 'menu.dashboard',
      items: [
        { icon: 'fa-duotone fa-mailbox', key: 'menu.mailbox', text: 'Mail Box', link: '/mailbox' , role: ['User','Admin']  }
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
