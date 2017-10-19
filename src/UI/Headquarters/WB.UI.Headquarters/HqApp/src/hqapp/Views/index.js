import HQ from "./HQ"
import Interviewer from "./Interviewer"

import ViewProvider from "../ComponentsProvider"

export default (rootStore) =>  new ViewProvider(rootStore, [HQ, Interviewer])