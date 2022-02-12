import Vue from 'vue'
import Router from 'vue-router'
// vuex
import store from '@/store'

// Vue Cookies
import Vuecookie from 'vue-cookies'

// Routes
import Routes from './app.routes'
import Pages from './pages.routes'
// Api
import { getMyInfo } from '@/api/auth'

Vue.use(Router)

export const routes = [{
  path: '/',
  redirect: '/mailbox'
},
...Pages,
...Routes,
{
  path: '/blank',
  name: 'blank',
  component: () => import(/* webpackChunkName: "blank" */ '@/pages/BlankPage.vue')
},
{
  path: '*',
  name: 'error',
  component: () => import(/* webpackChunkName: "error" */ '@/pages/error/NotFoundPage.vue'),
  meta: {
    layout: 'error'
  }
}]

const router = new Router({
  mode: 'history',
  base: process.env.BASE_URL || '/',
  scrollBehavior(to, from, savedPosition) {
    if (savedPosition) return savedPosition

    return { x: 0, y: 0 }
  },
  routes
})

/**
 * Before each route update
 */
router.beforeEach((to, from, next) => {
  if ( store.getters['auth/getIsAuth']) {
    return routerCheck(to ,next)
  } else {
    const token = Vuecookie.get('T') || '' //Get Token

    if (token !== '') {
      getMyInfo(token).then( (res) => {
        store.dispatch('auth/retriveToken', { token, userInfo: res.data.data })
          .then((r) => {
            return routerCheck(to ,next)
          })
      }).catch( () => {
        Vuecookie.delete('T')
        next('auth-signin')
      })
    } else {
      if (to.name === 'auth-signin') {
        return next()
      } else {
        return next({ name : 'auth-signin' })
      }
    }
  }

  return next()
})

// eslint-disable-next-line consistent-return
const routerCheck = (to ,next) => {
  switch (store.getters['auth/getUserInfo'].role) {
  case 'admin':
    return next()
  case 'user':
    if (to.path.startsWith('/mailbox')) {
      return next()
    } else {
      return next({
        path : '/clients'
      })
    }

  }
}

/**
 * After each route update
 */
router.afterEach((to, from) => {
})

export default router
