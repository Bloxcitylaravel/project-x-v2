import { createUseStyles } from 'react-jss';
import { useState, useEffect } from 'react';
import ActionCalls from './actionCalls';
import updatePlaceStore from "../stores/updatePlaceStore";
import { getAssetThumbnail } from '../../../services/thumbnails';

const useStyles = createUseStyles({
  contentContainer: {
    display: 'flex',
    flexDirection: 'column',
    paddingLeft: '12px',
    paddingRight: '12px',
    marginTop: '24px',
  },
  header: {
    '& h3': {
      fontWeight: 'bolder!important',
      marginBottom: '1.5rem!important',
      fontSize: '2rem',
      lineHeight: '1.2',
    },
  },
  mainContainer: {
    flex: '0 0 auto',
    display: 'flex',
    flexDirection: 'row',
  },
  iconContainer: {
    display: 'flex',
    flexDirection: 'column',
    borderRight: '1px solid #ccc',
    paddingRight: '20px'
  },
  gameIcon: {
    width: '100%!important',
    display: 'block!important',
    verticalAlign: 'middle',
    aspectRatio: '1/1',
  },
  noteText: {
    marginTop: '5px',
    marginBottom: '20px',
    fontSize: '10px',
    fontWeight: '500',
    lineHeight: '1.4em',
    display: 'block',
    width: '100%',
    fontStyle: 'italic',
    color: '#d2d2d2'
  },
  callsToAction: {
    display: 'flex',
    flexDirection: 'column',
    marginLeft: '20px',
    '& p': {
      marginBottom: '10px'
    }
  },
  feedback: {
    padding: '15px',
    backgroundColor: '#E2EEFE',
    border: '1px solid #6586A3',
    fontSize: '16px',
    fontWeight: '400',
    lineHeight: '1.4em',
  },



  footerContainer: {
    flex: '0 0 auto',
  },
})

function blobToBase64(blob) {
  return new Promise((resolve, reject) => {
    const reader = new FileReader();
    reader.onloadend = () => resolve(reader.result);
    reader.onerror = reject;
    reader.readAsDataURL(blob);
  });
}

const Icon = props => {
  const s = useStyles();
  const [feedback, setFeedback] = useState(null);
  const [gameIcon, setGameIcon] = useState('/placeholder_game_icon.png');
  const store = updatePlaceStore.useContainer();

  const refreshGameIcon = () => {
    setGameIcon('/placeholder_game_icon.png')
  }

  useEffect(() => {
    if (gameIcon == '/placeholder_game_icon.png') {
      getAssetThumbnail(
        store.placeId
      )
        .then((result) => {
          /*getImage(`projex.zip${result.data.data[0].imageUrl}`).then((img) => {
            blobToBase64(img.data).then(base64 => {
              setGameIcon(base64)
            })
          })*/
          setGameIcon(result.data.data[0].imageUrl)
        })
    }
  })

  return <div className={s.contentContainer}>
    <div className={`${s.header} col-12`}>
      <h3>Game Icon</h3>
    </div>
    <div className={`${s.mainContainer} col-12`}>
      <div className={`${s.iconContainer} col-8`}>
        <img className={s.gameIcon} src={gameIcon} />
        <p className={s.noteText}>Note: You can only have 1 icon per game.</p>
      </div>
      <div className={`${s.callsToAction} col-4`}>
        {feedback && <p className={s.feedback}>{feedback}</p>}
        <p style={{
          fontSize: '18px',
        }}>Change the Icon</p>
        <p style={{
          fontSize: '16px'
        }}>Media type:</p>
        <ActionCalls placeId={
          store.placeId
        } refreshIcon={refreshGameIcon} />
      </div>
    </div>
    <div className={`${s.footerContainer} col-12`}></div>
  </div>
};

export default Icon;