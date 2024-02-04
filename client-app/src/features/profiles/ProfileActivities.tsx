import { observer } from "mobx-react-lite";
import { SyntheticEvent, useEffect } from "react";
import { useStore } from "../../app/stores/store";
import { Card, Grid, Header, Image, Tab, TabPane, TabProps } from "semantic-ui-react";
import { Link } from "react-router-dom";
import { UserActivity } from "../../app/models/profile";
import { format } from "date-fns";

const panes = [
    {menuItem: 'Future Events', pane: { key: 'future' } },
    {menuItem: 'Past Events', pane: { key: 'past' } },
    {menuItem: 'Host', pane: { key: 'host' } },
];


export default observer(function ProfileActivities() {
    const {profileStore: {loadingActivities, loadUserActivities, userActivities, profile}} = useStore();

    useEffect(() => {
        loadUserActivities(profile!.username);
        }, [loadUserActivities, profile]);

    const handleTabChange = (_e: SyntheticEvent, data: TabProps) => {
        loadUserActivities(profile!.username, panes[data.activeIndex as number].pane.key);
    }

    return(
        <TabPane loading={loadingActivities}>
            <Grid>
                <Grid.Column width={16}>
                    <Header floated="left" icon='calendar' content={'Activities'} />
                </Grid.Column>
                <Grid.Column width={16}>
                    <Tab 
                        panes={panes}
                        menu={{ secondary: true, pointing: true}}
                        onTabChange={(e, data) => handleTabChange(e, data)}
                    />
                    <br />
                    <Card.Group itemsPerRow={4}>
                        {userActivities.map((activity: UserActivity) => (
                            <Card 
                                as={Link}
                                to={`/activities/${activity.id}`}
                                key={activity.id}
                            >
                                <Image 
                                    src={`/assets/categoryImages/${activity.category}.jpg`} 
                                    style={{minHeight:100, objectFit:'cover'}}
                                />
                                <Card.Content>
                                    <Card.Header textAlign="center">{activity.title}</Card.Header>
                                    <Card.Meta textAlign="center">
                                        <div>{format(new Date(activity.date),'dd LLL')}</div>
                                        <div>{format(new Date(activity.date),'h:mm a')}</div>
                                    </Card.Meta>
                                </Card.Content>
                            </Card>
                        ))}
                    </Card.Group>
                </Grid.Column>
            </Grid>
        </TabPane>
    )
})