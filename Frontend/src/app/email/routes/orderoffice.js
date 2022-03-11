export default [
  {
    path: '',
    redirect: 'inbox-received/internal'
  },
  {
    path: 'orders',
    name: 'apps-email-order-list',
    component: () => import('@/app/email/pages/OrderList.vue')
  }
]
