import stateAuth from '@/store/auth'

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

}

const showError = ({ state, commit }, message) => {
  if (state.toast.show) commit('hideToast')
  setTimeout(() => {
    commit('showToast', {
      color: 'error',
      message: message,
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
