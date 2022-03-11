export default [
  {
    path: 'users',
    name: 'apps-user-list',
    component: () => import( '@/app/user-list/UserList.vue')
  }
]
