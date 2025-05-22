import React from 'react';
import Head from 'next/head';
import Theme2016 from "../../../components/theme2016";
import UserProfile from "../../../components/userProfile";
import UserProfileStore from "../../../components/userProfile/stores/UserProfileStore";
import { getUserInfo } from "../../../services/users";
const UserProfilePage = ({ username, userId, description, ...props }) => {
  const ogTitle = username + "'s Profile" || "Project X";
  const ogUrl = userId ? `https://www.projex.zip/users/${userId}/profile` : '';
  const ogDesc = description || 'Join Project X and explore together!';

  return (
    <>
      {username && (
        <Head>
          <title>{ogTitle}</title>
          <meta property="og:title" content={ogTitle} />
          <meta property="og:url" content={ogUrl} />
          <meta property="og:type" content="profile" />
          <meta property="og:description" content={ogDesc} />
          <meta property="og:image" content={`https://www.projex.zip/thumbs/avatar-headshot.ashx?userId=${userId}`} />
          <meta name="og:site_name" content="Project X" />
          <meta name="theme-color" content="#f00000" />
        </Head>
      )}
      <UserProfileStore.Provider>
        <Theme2016>
          <UserProfile userId={userId}/>
        </Theme2016>
      </UserProfileStore.Provider>
    </>
  );
};

export async function getServerSideProps(context) {
  const { userId } = context.query;
  // we will get the username, desc
  try {
    const info = await getUserInfo({ userId });
    const username = info.name || "Project X"; 
    const description = info.description || "No description available";
    return {
      props: {
        username,
        description,
        userId
      }
    };
  } catch (error) {
    console.error("Error fetching user info");
    return {
      props: {
        username: "Project X", 
        description: "Join Project X and explore together!",
        userId
      }
    };
  }
}

export default UserProfilePage;
