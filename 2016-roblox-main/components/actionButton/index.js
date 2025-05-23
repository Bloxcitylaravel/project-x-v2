import { useState } from "react";
import { createUseStyles } from "react-jss";
import useButtonStyles from "../../styles/buttonStyles";

const useBuyButtonStyles = createUseStyles({
  btn: {
    textAlign: 'center',
    padding: '1px 13px 3px 13px',
    fontSize: '20px',
    color: 'white',
    border: '1px solid #357ebd',
    margin: '0 auto',
    display: 'block',
    fontWeight: 'normal',
    '&:disabled': {
      opacity: '0.5',
    },
  },
  wrapper: {
    width: '100%',
    border: '1px solid #a7a7a7',
    background: '#e1e1e1',
  },
  defaultBg: {
    background: 'linear-gradient(0deg, rgba(0,113,0,1) 0%, rgba(64,193,64,1) 100%)', // 40c140 #007100
    '&:hover': {
      background: 'linear-gradient(0deg, rgba(71,232,71,1) 0%, rgba(71,232,71,1) 100%)', // 47e847 02a101
    },
  },
});

const ActionButton = props => {
  const s2 = useBuyButtonStyles();
  const s = useButtonStyles();
  const bg = props.buttonStyle || s.buyButton;
  const type = props.type || "submit"

  return <div className={props.divClassName}>
    <button
      type={type}
      value={type}
      disabled={props.disabled}
      className={`${s2.btn} ${props.className} ${bg}`}
      onClick={props.onClick}
      title={props.disabled ? props.tooltipText : ''}>{props.label || 'Buy Now'}</button>
  </div>
}

export default ActionButton;
