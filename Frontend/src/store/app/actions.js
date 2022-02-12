import { getAllClient } from '@/api/client'
import stateAuth from '@/store/auth'
import { getAllStudy } from '@/api/study'

const showToast = ({ state, commit }, message) => {
  if (state.toast.show) commit('hideToast')

  setTimeout(() => {
    commit('showToast', {
      color: 'black' ,
      message,
      timeout: 3000
    })
  })
}

const notificationUpdate = async ({ state, commit }) => {
  const not = {
    unStudy : 0,
    WaDelivery : 0
  }

  await getAllStudy( stateAuth['state'].token, '?statusStudy=new&statusStudy=inProgress').then((res) => {
    not.unStudy = res.data.results

  })

  await getAllStudy( stateAuth['state'].token, '?statusStudy=complete').then((res) => {
    not.WaDelivery = res.data.results
  })
  setTimeout(() => {

    commit('setNotificationUnStudy', not.unStudy)
    commit('setNotificationWaDelivery', not.WaDelivery)
  })
}

const showError = (state, commit) => {
  if (state.toast.show) commit('hideToast')

  setTimeout(() => {
    commit('showToast', {
      color: 'error',
      message: message + ' ' + error,
      timeout: 3000
    })
  })
}

const showSuccess = ({ state, commit }, message) => {
  if (state.toast.show) commit('hideToast')

  setTimeout(() => {
    commit('showToast', {
      color: 'success',
      message,
      timeout: 3000
    })
  })
}

export default {
  showToast,
  showError,
  showSuccess,
  notificationUpdate
}
