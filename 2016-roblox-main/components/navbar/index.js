import React, { useRef } from "react";
import { createUseStyles } from "react-jss";
import { getTheme, themeType } from "../../services/theme";
import AuthenticationStore from "../../stores/authentication";
import NavSideBar from "../navSidebar";
import LoggedInArea from "./components/loggedinArea";
import LoginArea from "./components/loginArea";
import Logo from "./components/logo";
import NavigationLinks from "./components/navigationLinks";
import Search from "./components/search";

const useNavBarStyles = createUseStyles({
  navbar: {
    backgroundColor: p => p.theme === themeType.obc2016 ? '#393939' : '#0074BD',
    paddingTop: '0!important',
    paddingBottom: '0!important',
    display: 'block'
  },
  navContainer: {
    maxWidth: '100%!important',
    padding: 0,
    display: 'block!important'
  },
  leftContainer: {

  },
  row: {
    width: '100%',
    margin: '0!important',
    padding: '0!important'
  },
  wrapper: {
    marginBottom: '40px',
    maxWidth: '100vw',
    overflow: 'auto',
    '@media(max-width: 991px)': {
      marginBottom: '98px',
    }
  },
  rowOne: {
    display: 'block',
  },
  column: {
    margin: 0,
    padding: 0
  }
});

const Navbar = () => {
  const s = useNavBarStyles({
    theme: getTheme(),
  });
  const authStore = AuthenticationStore.useContainer();
  const mainNavBarRef = useRef(null);

  return <div className={s.wrapper + ' navbar-wrapper-main'}>
    <nav className={`navbar fixed-top navbar-expand-lg ${s.navbar}`} ref={mainNavBarRef}>
      <div className={`${s.navContainer} container`}>
        <div className={`${s.row} ${s.rowOne} row`}>
          <div className={`${s.column} col-12 col-lg-12`}>
            <div className={`${s.row} row`}>
              <Logo></Logo>
              <NavigationLinks></NavigationLinks>
              <Search></Search>
              {authStore.isPending ? null : authStore.isAuthenticated ? <LoggedInArea></LoggedInArea> : <LoginArea></LoginArea>}
            </div>
          </div>
        </div>
      </div>
    </nav>
    {authStore.isAuthenticated && <NavSideBar mainNavBarRef={mainNavBarRef}></NavSideBar>}
  </div>
}

export default Navbar;