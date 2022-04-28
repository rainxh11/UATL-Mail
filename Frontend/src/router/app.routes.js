import MailRoutes from '@/app/email/routes'

export default [
  {
    path: '/mailbox',
    name: 'mail-box',
    component: () => import(/* webpackChunkName: "mail-box" */ '@/app/email/EmailApp'),
    children: [
      ...MailRoutes
    ]  
  },
  {
    path: '/files',
    name: 'files-viewer',
    component: () => import(/* webpackChunkName: "files-viewer" */ '@/app/email/pages/AttachmentList'),
    children: []  
  },
  {
    path: '/setting/users',
    name: 'user-list',
    component: () => import(/* webpackChunkName: "user-list" */ '@/app/user/UserList'),
    children: []  
  }
]
