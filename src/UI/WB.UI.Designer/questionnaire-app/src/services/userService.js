import axios from 'axios';
import utilityService from './utilityService';

axios.defaults.headers.common['Content-Type'] = 'application/json';

class userService{
  urlBase = '../../api/users';  

  async getCurrentUserName() {
    var url = utilityService.format('{0}{1}', this.urlBase, '/CurrentLogin');
    const response = await axios.get(url);

    return response.data;
  };
}

export default new userService()
