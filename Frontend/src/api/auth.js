import axios from '@/api'

function headerAuth(token) {
  return {
    headers: {
      'Authorization': `Bearer ${token}`,
      'content-Type': 'Application/json'
    }
  }
}

// GET Api for { Authentification }
const signIn = async (email, password) => {
  return await axios.post('/users/login', { email, password }) }

const signOut = async (user) => {
  return await axios.post('/users/logout',  user ) }

const getMyInfo = async (token) => {
  return await axios.get('/users/me', headerAuth(token) )  }

const signUp = async (name, email, password) => {
  return await axios.post('/users/signup', { name:name, email: email, password: password, passwordConfirm: password }) }

const firstTimeAdmin = async () => {
  return await axios.post('/users/firsttimeAdmin') }

export {
  signIn, getMyInfo, signUp, firstTimeAdmin, signOut
}
