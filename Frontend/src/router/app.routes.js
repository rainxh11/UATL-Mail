import MailRoutes from '@/app/email/routes'

export default [
  {
    path: '/mailbox',
    name: 'mail-box',
    component: () => import(/* webpackChunkName: "landing-home" */ '@/app/email/EmailApp'),
    children: [
      ...MailRoutes
    ]  }
]
