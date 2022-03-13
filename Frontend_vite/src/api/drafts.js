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

// GET Api for { User }
const getAllDrafts = async (params, token) =>  {
  const defaultParams = {
    page: 1,
    limit: -1,
    direction: 'Received',
    type: 'Internal',
    sort: 'CreatedOn',
    desc: true
  }
  return await axios.get('/draft', {
    ...headerAuth(token),
    params: {
      ...defaultParams,
      ...params
    }
  })
}

const searchDrafts = async (search, params, token) =>  {
  const defaultParams = {
    page: 1,
    limit: -1,
    direction: 'Received',
    type: 'Internal',
    sort: 'CreatedOn',
    desc: true
  }

  return await axios.get('/draft/search', {
    ...headerAuth(token),
    params: {
      ...defaultParams,
      ...params,
      search: search ?? '|'
    } })
}

const getDraft = async (id, token) =>  await axios.get('/draft/' + id, headerAuth(token))

const deleteDraft = async (id, token) => await axios.delete('/draft/' + id, headerAuth(token))

const createDraft = async (draft, files, token) => {
  console.log(JSON.stringify(draft))

  return await axios.post('/draft', formData(draft, files), headerAuth(token))
}

export {
  getDraft, getAllDrafts, searchDrafts, deleteDraft, createDraft
}
