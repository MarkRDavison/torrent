import NavBar from './components/NavBar';
import Dashboard from './components/Dashboard';
import { Provider } from 'react-redux';
import store from './store/Store';
import AlertBar from './components/AlertBar';
import Shows from './components/Shows';
import { Route, Switch } from 'react-router-dom';
import SettingsPage from './components/SettingsPage';
import PrivateRoute from './components/PrivateRoute';
import Home from './components/Home';

const AppWithRouterAccess = (): JSX.Element => {
  
    return (
      <Provider store={store}>
          <NavBar />
          <AlertBar />
          <Switch>
              <PrivateRoute path='/dashboard' component={Dashboard}/>
              <PrivateRoute path='/shows' component={Shows} />
              <PrivateRoute path='/settings' component={SettingsPage}/>
              <Route path='/' component={Home} />
          </Switch>
      </Provider>
    );
};

export default AppWithRouterAccess;