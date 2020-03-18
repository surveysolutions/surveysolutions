import HQ from './HQ'
import Interviewer from './Interviewer'
import ViewProvider from '../ComponentsProvider'
import Progress from './progress'

export default (rootStore) =>  new ViewProvider(rootStore, [HQ, Interviewer, Progress])