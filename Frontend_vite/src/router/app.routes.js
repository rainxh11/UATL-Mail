import MailRoutes from '@/app/email/routes'

export default [
  {
    path: '/mailbox',
    name: 'mail-box',
    component: () => import('@/app/email/EmailApp.vue'),
    children: [
      ...MailRoutes
    ]  
  },
  {
    path: '/files',
    name: 'files-viewer',
    component: () => import('@/layouts/ErrorLayout.vue'),
    children: []  
  },
  {
    path: '/setting/users',
    name: 'user-list',
    component: () => import('@/app/user/UserList.vue'),
    children: []  
  }
]
