import { useRouter } from "next/router";
import UserInventory from "../../../components/userInventory";
import { getUserInfo } from "../../../services/users";
import Head from "next/head";

const UserInventoryPage = ({ username, userId }) => {
  const router = useRouter();
  const ogTitle = username ? `${username}'s Inventory` : "Project X";
  const ogUrl = userId ? `https://www.projex.zip/users/${userId}/inventory` : '';

  return (
    <>
      {username && (
        <Head>
          <title>{ogTitle}</title>
          <meta property="og:title" content={ogTitle} />
          <meta property="og:url" content={ogUrl} />
          <meta property="og:type" content="profile" />
          <meta property="og:description" content={`View ${username}'s inventory`} />
          <meta property="og:image" content={`https://www.projex.zip/thumbs/avatar-headshot.ashx?userId=${userId}`} />
          <meta name="og:site_name" content="Project X" />
          <meta name="theme-color" content="#f00000" />
        </Head>
      )}
      <UserInventory userId={userId} mode='Inventory' />
    </>
  );
}

export async function getServerSideProps(context) {
  const { userId } = context.query;
  try {
    const info = await getUserInfo({ userId });
    const username = info.name || null; 
    return {
      props: {
        username,
        userId
      }
    };
  } catch (error) {
    console.error("Error fetching user info");
    return {
      props: {
        username: null, 
        userId
      }
    };
  }
}

export default UserInventoryPage;
