import axios from '@/api'

function headerAuth(token) {
  return {
    headers: {
      'Authorization': `Bearer ${token}`,
      'content-Type': 'Application/json'
    }
  }
}

// GET Api for { Study }
const getAll2fa = async (token, filter) => {
  return await axios.get('/2fa' + filter, headerAuth(token) )  }

const getOne2fa = async (id, token) => {
  return await axios.get('/2fa/' + id, headerAuth(token) )  }

const create2fa = async (twoFactor, token) => {
  return await axios.post('/2fa', twoFactor, headerAuth(token) )  }

const generate2fa = async (name, duration, token) => {
  const model = {
    name: name,
    duration: duration
  }

  return await axios.post('/2fa/generate/', model, headerAuth(token) )  }

//GENERATE 2FA TOKENS with & without ID
const generate2faToken = async (id, token) => {
  return await axios.post('/2fa/token/' + id, headerAuth(token) )  }

const generateToken = async (secret, token) => {
  return await axios.post('/2fa/token' + '?secret=' + secret, headerAuth(token) )  }

//VERIFY 2FA TOKENS with & without ID
const verify2faToken = async (id, code, token) => {
  return await axios.get('/2fa/verify/' + id + '?token=' + code, headerAuth(token) )  }

const verifyToken = async (secret, code, duration, token) => {
  const query = '?secret=' + secret + '&token=' + code + '&duration' + duration

  return await axios.get('/2fa/token' + query, headerAuth(token) )  }

const update2fa = async (id, twoFactor, token) => {
  return await axios.patch('/2fa/' + id, twoFactor, headerAuth(token) )
}

const delete2fa = async (id, token) => {
  return await axios.delete('/2fa/' + id, headerAuth(token) )
}
const count2fa = async (token) => {
  return await axios.get('/2fa/count', headerAuth(token) )}

export {
  getAll2fa, getOne2fa, generateToken, generate2faToken ,generate2fa, verifyToken, verify2faToken, delete2fa, update2fa, create2fa, count2fa
}
