import axios from '@/api'

function headerAuth(token) {
  return {
    headers: {
      'Authorization': `Bearer ${token}`,
      'content-Type': 'Application/json'
    }
  }
}

function formData(data, files) {
  const formData = new FormData()

  files.forEach((file) => {
    formData.append('files', file, file.name)
  })
  formData.append('value', JSON.stringify(data))

  return formData
}

const getAllMails = async (page, limit, token) =>  {
  const query = `?page=${page}&limit=${limit ?? -1}&sort=CreatedOn&desc=true`

  return await axios.get('/mail' + query, headerAuth(token))
}

const searchMails = async (query, token) =>  {
  query ??= '?page=1&limit=-1&sort=CreatedOn&desc=true&search=|'

  return await axios.get('/mail/search' + query, headerAuth(token))
}

const getMail = async (id, token) =>  await axios.get('/mail/' + id, headerAuth(token))

const sendMail = async (mail, files, token) => {
  return await axios.post('/mail', formData(mail, files), headerAuth(token))
}

export {
  searchMails, sendMail, getMail, getAllMails
}
