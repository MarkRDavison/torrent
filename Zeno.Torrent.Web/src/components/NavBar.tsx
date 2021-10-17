import React from 'react';
import { makeStyles } from '@material-ui/core/styles';
import AppBar from '@material-ui/core/AppBar';
import Toolbar from '@material-ui/core/Toolbar';
import Typography from '@material-ui/core/Typography';
import IconButton from '@material-ui/core/IconButton';
import MenuIcon from '@material-ui/icons/Menu';
import { Menu, MenuItem } from '@material-ui/core';
import { withRouter } from 'react-router';
import { useAuth } from './AuthContext';

const useStyles = makeStyles((theme) => ({
  root: {
    flexGrow: 1,
  },
  menuButton: {
    marginRight: theme.spacing(2),
  },
  title: {
    flexGrow: 1,
  },
}));

interface WithRouterProps {
  history: any
  location: any
  match: any
}
interface OwnProps {

}

type Props = WithRouterProps & OwnProps;

const _NavBar = (props: Props): JSX.Element => {
  const [anchorEl, setAnchorEl] = React.useState(null);
  const handleClick = (event: any) => {
      setAnchorEl(event.currentTarget);
  };

  const {
    isLoggedIn,
    user,
    login,
    logout
  } = useAuth();

  const handleClose = () => {
      setAnchorEl(null);
  };
  const classes = useStyles();

  const navigate = (page: string) => {
    props.history.push(page);
    handleClose();
  };

  return (
    <div className={classes.root}>
      <AppBar position="static">
        <Toolbar>
          <IconButton edge="start" className={classes.menuButton} color="inherit" aria-label="menu" onClick={handleClick}>
            <MenuIcon />
          </IconButton>
          <Menu
              id="application-menu"
              aria-label="application-menu"
              anchorEl={anchorEl}
              onClose={handleClose}
              open={anchorEl !== null}>
              <MenuItem onClick={() => navigate('/')}>Home</MenuItem>
              <MenuItem onClick={() => navigate('/dashboard')}>Dashboard</MenuItem>
              <MenuItem onClick={() => navigate('/shows')}>Shows</MenuItem>
              <MenuItem onClick={() => navigate('/settings')}>Settings</MenuItem>
              { (isLoggedIn ? <MenuItem onClick={logout}>Logout</MenuItem> : <MenuItem onClick={login}>Login</MenuItem>) }
          </Menu>
          <Typography variant="h5" className={classes.title}>
            Zeno Torrent Daemon
          </Typography>
          <Typography variant="h5" className={classes.title}>
            {(isLoggedIn ? user?.name : null)}
          </Typography>
        </Toolbar>
      </AppBar>
    </div>
  );
};

const NavBar = withRouter(_NavBar);
export default NavBar;