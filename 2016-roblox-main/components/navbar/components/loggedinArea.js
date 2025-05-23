import React, { useState } from "react";
import { createUseStyles } from "react-jss";
import getFlag from "../../../lib/getFlag";
import { logout } from "../../../services/auth";
import AuthenticationStore from "../../../stores/authentication";
import Link from "../../link";

const useDropdownStyles = createUseStyles({
  wrapper: {
    width: "125px",
    position: "absolute",
    top: "45px",
    right: "10px",
    boxShadow: "0 -5px 20px rgba(25,25,25,0.15)",
    userSelect: "none",
    background: "white",
  },
  text: {
    padding: "10px",
    marginBottom: 0,
    fontSize: "16px",
    "&:hover": {
      background: "#eaeaea",
      borderLeft: "4px solid #0074BD",
    },
    "&:hover > a": {
      marginLeft: "-4px",
    },
  },
});

const SettingsDropdown = (props) => {
  const authStore = AuthenticationStore.useContainer();
  const s = useDropdownStyles();
  return (
    <div className={s.wrapper}>
      <p className={`${s.text}`}>
        <Link href="/My/Account">
          <a className="text-dark">Settings</a>
        </Link>
      </p>
      <p className={`${s.text}`}>
        <Link href="/help">
          <a className="text-dark">Help</a>
        </Link>
      </p>
      <p className={`${s.text}`}>
        <a
          onClick={(e) => {
            e.preventDefault();
            logout().then(() => {
              window.location.reload();
            });
          }}
          className="text-dark"
        >
          Logout
        </a>
      </p>
    </div>
  );
};

const useLoginAreaStyles = createUseStyles({
  linkContainer: {
    display: "flex",
    margin: 0,
    marginRight: "20px",
    paddingLeft: '12px',
    paddingRight: '12px',
    justifyContent: 'flex-end',
    alignItems: 'center',
    "&:before": {
      content: '""',
      display: "table",
    },
    "&:after": {
      content: '""',
      display: "table",
      clear: "both",
    },
  },
  linkContainerCol: {
    width: "25%",
    //maxWidth: "600px",
    float: "right",
    marginLeft: "auto",
    marginRight: "3px",
  },
  row: {
    display: "block",
    height: "100%",
    "&:before": {
      content: '""',
      display: "table",
    },
    "&:after": {
      content: '""',
      display: "table",
      clear: "both",
    },
  },
  ageNameContainer: {
    display: "inline-block",
    float: "left",
    color: "#fff",
    marginRight: "0",
    fontSize: "12px",
    fontWeight: "500",
  },
  nameLink: {
    color: "inherit",
    display: "inline",
    textDecoration: "none",
  },
  nameSpan: {
    marginRight: 0,
    color: "inherit",
    display: "inline",
    fontWeight: '400',
    "&:after": {
      content: '": "',
    },
  },
  ageSpan: {
    color: "inherit",
    display: "inline",
    fontWeight: '400',
  },
  messagesContainer: {
    float: "left",
    height: "40px",
    marginLeft: "0",
    width: "auto",
    textAlign: "center",
    listStyle: "none",
    display: "flex",
    position: "relative",
  },
  messagesLink: {
    padding: "6px 9px",
    display: "inline",
    position: "relative",
    marginRight: "0",
  },
  currencyContainer: {
    padding: "0",
    display: "block",
    cursor: "pointer",
    float: "left",
    position: "relative",
  },
  tixContainer: {
    //paddingRight: '9px'
  },
  robuxContainer: {
    paddingLeft: '6px'
  },
  currencyIcon: {
    verticalAlign: "middle",
  },
  currencyPrice: {
    display: "inline-block",
    minWidth: "42px",
    fontWeight: "300",
    textAlign: "center",
    margin: 0,
    marginLeft: "5px",
    textDecoration: "none",
    color: "#fff",
    float: "right",
    marginBottom: "auto",
    position: "relative",
    userSelect: 'none',
    '&:link': { textDecoration: 'none!important', color: '#fff' },
    '&:visited': { textDecoration: 'none!important', color: '#fff' },
    '&:hover': { textDecoration: 'none!important', color: '#fff' },
    '&:active': { textDecoration: 'none!important', color: '#fff' },
  },
  currencyLink: {
    display: "flex",
    width: "100%",
    height: "100%",
    justifyContent: 'center',
    alignItems: 'center',
  },
  settingsIcon: {
    float: "right",
  },
  text: {
    color: "white",
    fontWeight: 400,
    fontSize: "16px",
    borderBottom: 0,
    textAlign: "right",
    whiteSpace: "nowrap",
    display: "inline",
    marginRight: '0'
  },

  currencySpan:{
    color: '#fff',
    display: 'inline',
    marginLeft: '5px',
    marginRight: '14px',
    fontSize: '16px',
    height: '100%',
    marginBottom: '2px'
  },
});

const LoggedInArea = (props) => {
  const s = useLoginAreaStyles();
  const authStore = AuthenticationStore.useContainer();
  const [settingsOpen, setSettingsOpen] = useState(false);

  if (authStore.robux === null || authStore.tix === null) return null;
  return (
    <div className={`${s.linkContainerCol} `}>
      <div className={`${s.row} row`}>
        <ul className={`${s.linkContainer}`}>
          <div className={`${s.ageNameContainer}`}>
            <a
              href={`/users/${authStore.userId}/profile`}
              className={`${s.nameLink}`}
            >
              <span className={`${s.nameSpan}`}>{authStore.username}</span>
            </a>
            <span className={`${s.ageSpan}`}>13+</span>
          </div>
          <li className={`${s.messagesContainer}`}>
            <a href="/My/Messages" className={`${s.messagesLink}`}>
              <span className="icon-nav-message2" />
            </a>
          </li>
          <li className={`${s.currencyContainer} ${s.robuxContainer}`}>
            <a className={`${s.currencyLink}`} href='/My/Money.aspx'>
              <span className={`${s.currencyIcon} icon-nav-robux`} />
              <span className={s.currencySpan}>
                {authStore.robux.toLocaleString()}
              </span>
            </a>
          </li>
          {getFlag("showTicketBalace", false) ? (
            <>
              <li className={`${s.currencyContainer} ${s.tixContainer}`}>
                <a className={`${s.currencyLink}`} href='/My/Money.aspx'>
                  <span className="icon-nav-tix" />
                  <span className={s.currencySpan}>
                    {authStore.tix.toLocaleString()}
                  </span>
                </a>
              </li>
            </>
          ) : null}
          <li className={`${s.text}`}>
            <a
              onClick={(e) => {
                e.preventDefault();
                setSettingsOpen(!settingsOpen);
              }}
            >
              <span
                className={`icon-nav-settings ${s.settingsIcon}`}
                id="nav-settings"
              ></span>
            </a>
          </li>
          {settingsOpen && <SettingsDropdown />}
        </ul>
      </div>
    </div>
  );
};

export default LoggedInArea;
