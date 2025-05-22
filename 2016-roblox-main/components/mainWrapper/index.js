import { createUseStyles } from "react-jss"

const useStyles = createUseStyles({
  main: {
    minHeight: '95vh',
    paddingTop: '12px',
  }
})

const MainWrapper = ({ children }) => {
  const s = useStyles();
  return <div className={s.main}>
    {children}
  </div>
}

export default MainWrapper;