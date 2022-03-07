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
    sort: 'SentOn',
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
    sort: 'SentOn',
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

const getStarred = async (full) => await axios.get('/mail/starred?full=' + full)

const addStarred = async (ids) => await axios.post('/mail/starred', ids)

const updateStarred = async (ids) => await axios.patch('/mail/starred', ids)

const deleteStarred = async (id) => await axios.delete('/mail/starred/' + id)

const getStarredFull = async (params, token) =>  {
  const defaultParams = {
    full: true,
    page: 1,
    limit: -1,
    sort: 'SentOn',
    desc: true
  }
  return await axios.get('/mail/starred',
    {
      ...headerAuth(token),
      params: {
        ...defaultParams,
        ...params
      }
    })
}

const searchStarredFull = async (search, params, token) =>  {
  const defaultParams = {
    search: search,
    full: true,
    page: 1,
    limit: -1,
    sort: 'SentOn',
    desc: true
  }
  return await axios.get('/mail/starred',
    {
      ...headerAuth(token),
      params: {
        ...defaultParams,
        ...params
      }
    })
}

const getTags = async () => await axios.get('/mail/tags')
const searchTags = async (search = '') => await axios.get('/mail/tags?search=' + search)

const getTaggedMails = async (tag, params, token) =>  {
  const defaultParams = {
    page: 1,
    limit: -1,
    sort: 'SentOn',
    desc: true
  }
  return await axios.get('/mail/tagged',
    {
      ...headerAuth(token),
      params: {
        ...defaultParams,
        ...params,
        tag: tag
      }
    })
}

const searchTaggedMails = async (tag, search, params, token) =>  {
  const defaultParams = {
    page: 1,
    limit: -1,
    sort: 'SentOn',
    desc: true
  }
  return await axios.get('/mail/tagged/search', {
    ...headerAuth(token),
    params: {
      ...defaultParams,
      ...params,
      tag: tag,
      search: search ?? '|'
    } })
}

const getMailWithReplies = async(id) => await axios.get(`/mail/${id}/replies`)

export {
  searchMails,
  sendMail,
  getMail,
  getAllMails,
  getStats,
  getStarred,
  updateStarred,
  addStarred,
  deleteStarred,
  getStarredFull,
  searchStarredFull,
  getTags,
  searchTags,
  getTaggedMails,
  searchTaggedMails,
  getMailWithReplies
}
