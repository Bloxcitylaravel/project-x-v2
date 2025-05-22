import { createUseStyles } from "react-jss";
import Link from "../../link";
import { useEffect } from "react";
import { useRouter } from 'next/router';
const useStyles = createUseStyles({
  container: {
    marginTop: '0',
    marginBottom: 0,
    paddingBottom: 0,
    paddingLeft: '0',
    width: '100%!important',
    height: '100%!important'
  },
  linkEntry: {
    color: 'white',
    fontWeight: 500,
    margin: 'auto',
    textAlign: 'center',
    fontSize: '16px',
    display: 'inline',
    textDecoration: 'none',
    padding: '6px 9px!important',
    transition: 'none',
    '&:hover': {
      color: 'white',
      background: 'rgba(25,25,25,0.1)',
      cursor: 'pointer',
      borderRadius: '5px',
      transition: 'none',
    },
  },
  navItem: {
    paddingRight: '2rem',
    '@media(max-width: 1300px)': {
      paddingRight: '1.75rem',
    },
    '@media(max-width: 1250px)': {
      paddingRight: '1.5rem',
    },
    '@media(max-width: 1175px)': {
      paddingRight: '1rem',
    },
  },
  col: {
    paddingLeft: 0,
    marginLeft: 0,
  },
  row: {
    margin: 0,
    height: '100%'
  },
  linkContainer: {
    display: 'flex',
    margin: 0,
    padding: 0
  }
})

const LinkEntry = props => {
  const s = useStyles();
  return <div className={`${s.linkContainer} col-3`}>
    <Link href={`/${props.url}`}>
      <a className={`${s.linkEntry} nav-link active pt-0`}>
        {props.children}
      </a>
    </Link>
  </div>
}

const NavigationLinks = props => {
  const s = useStyles();
  const router = useRouter();

  useEffect(() => {
    const checkAuth = () => {
      const robloSecurity = document.cookie.split(';').find(cookie => cookie.trim().startsWith('.ROBLOSECURITY='));
      if (!robloSecurity) {
        router.push('/auth/login');
      }
    };

    checkAuth();
  }, [router]);
  return <div className={`${s.col} col-10 col-lg-3`}>
    <div className={s.container}>
      <div className={`${s.row} row`}>
        <LinkEntry url='games'>Games</LinkEntry>
        <LinkEntry url='catalog'>Catalog</LinkEntry>
        <LinkEntry url='develop'>Create</LinkEntry>
        <LinkEntry url='download'>Download</LinkEntry>
      </div>
    </div>
  </div>
}

export default NavigationLinks;