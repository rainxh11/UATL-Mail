export default [
  {
    path: '',
    redirect: 'inbox-received/internal'
  },
  {
    path: 'tagged',
    name: 'apps-email-tagged',
    component: () => import(/* webpackChunkName: "apps-email-inbox" */ '@/app/email/pages/TaggedPage.vue')
  },
  {
    path: 'inbox-:direction(received)/:internal(.*)',
    name: 'apps-email-inbox',
    component: () => import(/* webpackChunkName: "apps-email-inbox" */ '@/app/email/pages/InboxPage.vue')
  },
  {
    path: 'inbox-:direction(received)/:internal(.*)',
    name: 'apps-email-inbox',
    component: () => import(/* webpackChunkName: "apps-email-inbox" */ '@/app/email/pages/InboxPage.vue')
  },
  {
    path: 'inbox-:direction(sent)/:internal(.*)',
    name: 'apps-email-sent',
    component: () => import(/* webpackChunkName: "apps-email-sent" */ '@/app/email/pages/SentPage.vue')
  },
  {
    path: 'inbox-:direction(sent)/:internal(.*)',
    name: 'apps-email-sent',
    component: () => import(/* webpackChunkName: "apps-email-sent" */ '@/app/email/pages/SentPage.vue')
  },
  {
    path: 'inbox-:type(drafts)',
    name: 'apps-email-drafts',
    component: () => import(/* webpackChunkName: "apps-email-drafts" */ '@/app/email/pages/DraftsPage.vue')
  },
  {
    path: 'inbox-:type(starred)',
    name: 'apps-email-starred',
    component: () => import(/* webpackChunkName: "apps-email-starred" */ '@/app/email/pages/StarredPage.vue')
  },
  {
    path: ':type(mail|draft)/:id',
    name: 'apps-email-inbox',
    component: () => import(/* webpackChunkName: "apps-email-inbox" */ '@/app/email/pages/ViewPage.vue')
  },
  {
    path: 'inbox',
    name: 'apps-email-inbox',
    component: () => import(/* webpackChunkName: "apps-email-inbox" */ '@/app/email/pages/InboxPage.vue')
  }
]
