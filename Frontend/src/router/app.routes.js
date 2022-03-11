import MailRoutes from '@/app/email/routes'

export default [
  {
    path: '/mailbox',
    name: 'mail-box',
    component: () => import(/* webpackChunkName: "landing-home" */ '@/app/email/EmailApp'),
    children: [
      ...MailRoutes
    ]  
  },
  {
    path: '/files',
    name: 'files-viewer',
    component: () => import(/* webpackChunkName: "landing-home" */ '../layouts/ErrorLayout'),
    children: []  
  },
  {
    path: '/setting/users',
    name: 'user-list',
    component: () => import(/* webpackChunkName: "landing-home" */ '@/app/user/UserList'),
    children: []  
  }
]
