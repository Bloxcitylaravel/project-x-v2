import { createUseStyles } from "react-jss";
import NavigationStore from "../../../stores/navigation";

const useLogoStyles = createUseStyles({
  imgDesktop: {
    width: '118px',
    minWidth: '118px',
    maxWidth: '118px',
    height: '40px',
    backgroundImage: `url(/img/roblox_logo.svg)`,
    backgroundSize: '118px 30px',
    display: 'none',
    '@media(min-width: 1301px)': {
      display: 'block',
    },
    backgroundRepeat: 'no-repeat',
    backgroundPosition: 'center'
  },
  imgMobile: {
    backgroundImage: `url(/img/logo_R.svg)`,
    width: '30px',
    height: '30px',
    display: 'block',
    backgroundSize: '30px',
    '@media(min-width: 1301px)': {
      display: 'none',
    },
  },
  imgMobileWrapper: {
    marginLeft: '12px',
  },
  col: {
    maxWidth: '118px',
    padding: '0',
    margin: '0 12px',
    display: 'flex',
    justifyContent: 'start',
    alignItems: 'center',
    '@media(min-width: 992px)':{
      width: '6%'
    }
  },
  openSideNavMobile: {
    display: 'none',
    '@media(max-width: 1300px)': {
      display: 'block',
      float: 'left',
      height: '30px',
      width: '30px',
      cursor: 'pointer',
    },
  },
});
const Logo = () => {
  const s = useLogoStyles();
  const navStore = NavigationStore.useContainer();

  return <div className={`${s.col} col-3 col-lg-3`}>
    <div className={s.openSideNavMobile + ' icon-menu'} onClick={() => {
      navStore.setIsSidebarOpen(!navStore.isSidebarOpen);
    }}></div>
    <a className={s.imgDesktop} href='/home'></a>
    <div className={s.imgMobileWrapper}>
      <div className={s.imgMobile}></div>
    </div>
  </div>
}

export default Logo;