export default {
  // main navigation - side menu
  menu: [
    {
      text: 'Command Center',
      role: ['Admin','User'],
      key: 'menu.dashboard',
      items: [
        { icon: 'fa-regular fa-mailbox', key: 'menu.mailbox', text: 'Mail Box', link: '/mailbox' , role: ['User','Admin']  },
        { icon: 'fa-regular fa-files', key: 'menu.fileExplorer', text: 'Mail Box', link: '/files' , role: ['User','Admin']  }
      ]
    },
    {
      text: 'Setting',
      role: ['Admin'],
      key: 'menu.setting',
      items: [
        { icon: 'fa-regular fa-users-gear', key: 'menu.users', text: 'Mail Box', link: '/setting/users' , role: ['User','Admin']  }
      ]
    }
  ],

  // footer links
  footer: [{
    text: 'Docs',
    key: 'menu.docs',
    href: 'https://vuetifyjs.com',
    target: '_blank'
  }]
}
