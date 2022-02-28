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

const getAllMails = async (params, token) =>  {
  const defaultParams = {
    page: 1,
    limit: -1,
    direction: 'Received',
    type: 'Internal',
    sort: 'CreatedOn',
    desc: true
  }
  return await axios.get('/mail',
    {
      ...headerAuth(token),
      params: {
        ...defaultParams,
        ...params
      }
    })
}

const searchMails = async (search, params, token) =>  {
  const defaultParams = {
    page: 1,
    limit: -1,
    direction: 'Received',
    type: 'Internal',
    sort: 'CreatedOn',
    desc: true
  }
  return await axios.get('/mail/search', {
    ...headerAuth(token),
    params: {
      ...defaultParams,
      ...params,
      search: search ?? '|'
    } })
}

const getMail = async (id, token) =>  await axios.get('/mail/' + id, headerAuth(token))

const sendMail = async (mail, files, onProgress, token, ct) => {
  return await axios.post('/mail', formData(mail, files), {
    ...headerAuth(token),
    cancelToken: ct.token,
    onUploadProgress: onProgress
  })
}

const getStats = async (token) => await axios.get('/mail/stats', headerAuth(token))

export {
  searchMails, sendMail, getMail, getAllMails, getStats
}
