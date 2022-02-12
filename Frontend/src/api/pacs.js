import axios from '@/api'
import dicomServer from './axios_dicomserver'

function headerAuth(token,type) {
  return {
    headers: {
      'Content-Type': 'multipart/form-data'
    }
  }
}

const getStudies = async(dicomServerUrl) => {
  return await dicomServer.get(dicomServerUrl + '/studies', headerAuth())
}

const getJobs = async(dicomServerUrl) => {
  return await dicomServer.get(dicomServerUrl + '/jobs', headerAuth())
}
const resumeJob = async(dicomServerUrl, id) => {
  return await dicomServer.post(dicomServerUrl + '/jobs/' + id + '/resume', headerAuth())
}
const pauseJob = async(dicomServerUrl, id) => {
  return await dicomServer.post(dicomServerUrl + '/jobs/' + id + '/pause', headerAuth())
}
const cancelJob = async(dicomServerUrl, id) => {
  return await dicomServer.post(dicomServerUrl + '/jobs/' + id + '/cancel', headerAuth())
}
const resubmitJob = async(dicomServerUrl, id) => {
  return await dicomServer.post(dicomServerUrl + '/jobs/' + id + '/resubmit', headerAuth())
}

const getJob = async(dicomServerUrl, id) => {
  return await dicomServer.get(dicomServerUrl + '/jobs/' + id, headerAuth())
}
const getJobArchive = async(dicomServerUrl, id) => {
  return await dicomServer.get(dicomServerUrl + '/jobs/' + id + '/archive', headerAuth())
}

const queueArchiveJob = async(dicomServerUrl, id) => {
  return await dicomServer.post(dicomServerUrl + '/studies/' + id + '/archive', headerAuth())
}

const deleteStudy = async(dicomServerUrl, id) => {
  return await dicomServer.delete(dicomServerUrl + '/studies/' + id, headerAuth())
}

const searchStudies = async(dicomServerUrl, query) => {
  return await dicomServer.get(dicomServerUrl + '/studies/search' + query, headerAuth())
}

const getStudy = async(dicomServerUrl, id) => {
  return await dicomServer.get(dicomServerUrl + '/studies/' + id, headerAuth())
}

const getStorageStatus = async(dicomServerUrl, id) => {
  return await dicomServer.get(dicomServerUrl + '/pacs/status', headerAuth())
}

export {
  getStudies,
  getStudy,
  searchStudies,
  getStorageStatus,
  deleteStudy,
  getJobs,
  getJob,
  getJobArchive,
  queueArchiveJob,
  cancelJob,
  resubmitJob,
  resumeJob,
  pauseJob
}
