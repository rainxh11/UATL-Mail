import axios from '@/api'

function headerAuth(token) {
  return {
    headers: {
      'Authorization': `Bearer ${token}`,
      'content-Type': 'Application/json'
    }
  }
}

// GET Api for { Doctor }
const getAllDoctor = async (token) => {
  return await axios.get('/doctors', headerAuth(token) )  }

const createDoctor = async (doctorInfo, token) => {
  return await axios.post('/doctors/', doctorInfo, headerAuth(token) )  }

export {
  getAllDoctor  , createDoctor
}
