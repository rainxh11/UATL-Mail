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
const getAllDrafts = async (page, limit, token) =>  {
  const query = `?page=${page}&limit=${limit ?? -1}&sort=CreatedOn&desc=true`

  return await axios.get('/draft' + query, headerAuth(token))
}

const searchDrafts = async (query, token) =>  {
  query ??= '?page=1&limit=-1&sort=CreatedOn&desc=true&search=|'

  return await axios.get('/draft/search' + query, headerAuth(token))
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
